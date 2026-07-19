import requests
import urllib3
import json

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

BASE_URL = 'https://localhost:7241'
USERS = [
    {'email': 'Mahmoud@gmail.com', 'password': 'Mahmoud1@gmail.com'},
    {'email': 'Mahmoud1@gmail.com', 'password': 'Mahmoud1@gmail.com'}
]

# Manager test account (seeded by the application initializer)
MANAGER = {'email': 'manager@lapbox.local', 'password': 'Manager1!'}

HEADERS = {'Content-Type': 'application/json'}


def post(path, payload):
    return requests.post(f'{BASE_URL}{path}', json=payload, headers=HEADERS, verify=False)


def get(path, token=None):
    headers = {'Authorization': f'Bearer {token}'} if token else {}
    return requests.get(f'{BASE_URL}{path}', headers=headers, verify=False)


def test_user(user):
    print('\n=== USER:', user['email'], '===')
    login_resp = post('/api/auth/login', user)
    print('LOGIN STATUS:', login_resp.status_code)
    print(login_resp.text)

    if login_resp.status_code == 200:
        body = login_resp.json()
        access_token = body.get('accessToken')
        refresh_token = body.get('refreshToken')

        me_resp = get('/api/auth/me', access_token)
        print('ME STATUS:', me_resp.status_code)
        print(me_resp.text)

        my_orders_resp = get('/api/v1/orders/my-orders', access_token)
        print('MY ORDERS STATUS:', my_orders_resp.status_code)
        print(my_orders_resp.text)

        all_orders_resp = get('/api/v1/orders', access_token)
        print('ALL ORDERS STATUS:', all_orders_resp.status_code)
        print(all_orders_resp.text)

        refresh_resp = post('/api/auth/refresh', {'accessToken': access_token, 'refreshToken': refresh_token})
        print('REFRESH STATUS:', refresh_resp.status_code)
        print(refresh_resp.text)
    else:
        print('Login failed. Attempting registration...')
        name = user['email'].split('@')[0].capitalize()
        register_data = {
            'firstName': name,
            'lastName': 'Test',
            'email': user['email'],
            'password': user['password']
        }
        register_resp = post('/api/auth/register', register_data)
        print('REGISTER STATUS:', register_resp.status_code)
        print(register_resp.text)
        if register_resp.status_code == 201:
            print('Registration succeeded, please rerun login manually or rerun this script.')


if __name__ == '__main__':
    for user in USERS:
        test_user(user)

    # Test manager actions
    print('\n=== MANAGER TEST ===')
    m = MANAGER
    login_resp = post('/api/auth/login', m)
    print('MANAGER LOGIN STATUS:', login_resp.status_code)
    print(login_resp.text)
    if login_resp.status_code == 200:
        token = login_resp.json().get('accessToken')
        # Try create brand (ManagerOnly)
        # Use None -> JSON null; prepare payload accordingly
        import json as _json
        payload = {'name': 'TestBrand', 'description': 'Created by integration test', 'logoUrl': None}
        r = requests.post(f'{BASE_URL}/api/v1/catalog/brands', json=payload, headers={'Authorization': f'Bearer {token}', 'Content-Type': 'application/json'}, verify=False)
        print('CREATE BRAND STATUS:', r.status_code)
        print(r.text)
        # Fetch brands and categories to get IDs
        b = requests.get(f'{BASE_URL}/api/v1/catalog/brands', headers={'Authorization': f'Bearer {token}'}, verify=False)
        c = requests.get(f'{BASE_URL}/api/v1/catalog/categories', headers={'Authorization': f'Bearer {token}'}, verify=False)
        print('BRANDS LIST STATUS:', b.status_code)
        print('CATEGORIES LIST STATUS:', c.status_code)
        try:
            brands = b.json()
            cats = c.json()
            brand_id = brands[0]['id'] if brands else None
            cat_id = cats[0]['id'] if cats else None
        except Exception:
            brand_id = None
            cat_id = None

        if brand_id and cat_id:
            laptop_payload = {
                'brandId': brand_id,
                'categoryId': cat_id,
                'name': 'Test Laptop',
                'description': 'Integration test laptop',
                'basePrice': 999.99,
                'inventoryQuantity': 10,
                'processor': 'i7-13700',
                'ram': '16GB',
                'storage': '512GB',
                'screenSize': '15.6',
                'graphicsCard': 'RTX 4060'
            }
            r2 = requests.post(f'{BASE_URL}/api/v1/laptops', json=laptop_payload, headers={'Authorization': f'Bearer {token}', 'Content-Type': 'application/json'}, verify=False)
            print('CREATE LAPTOP STATUS:', r2.status_code)
            print(r2.text)
            laptop_id = None
            if r2.status_code in (200,201):
                try:
                    laptop_id = r2.json().get('id')
                except Exception:
                    laptop_id = None
        else:
            laptop_id = None

        # As customer, add to cart and place order
        cust = {'email': 'Mahmoud1@gmail.com', 'password': 'Mahmoud1@gmail.com'}
        login_cust = post('/api/auth/login', cust)
        if login_cust.status_code == 200 and laptop_id:
            cust_token = login_cust.json().get('accessToken')
            add_item = {'laptopId': laptop_id, 'quantity': 1}
            r_add = requests.post(f'{BASE_URL}/api/v1/cart/items', json=add_item, headers={'Authorization': f'Bearer {cust_token}', 'Content-Type': 'application/json'}, verify=False)
            print('ADD TO CART STATUS:', r_add.status_code)
            print(r_add.text)

            place_payload = {'customerId': None, 'promotionCode': None, 'street': '1 Test St', 'city': 'Testville', 'state': 'TS', 'zipCode': '12345', 'country': 'Testland'}
            r_order = requests.post(f'{BASE_URL}/api/v1/orders/place', json=place_payload, headers={'Authorization': f'Bearer {cust_token}', 'Content-Type': 'application/json'}, verify=False)
            print('PLACE ORDER STATUS:', r_order.status_code)
            print(r_order.text)
            # Verify order appears
            my_orders = requests.get(f'{BASE_URL}/api/v1/orders/my-orders', headers={'Authorization': f'Bearer {cust_token}'}, verify=False)
            print('MY ORDERS AFTER PLACE STATUS:', my_orders.status_code)
            print(my_orders.text)
        else:
            print('Skipping cart/order tests — missing laptop or customer login failed.')
    else:
        print('Manager login failed; ensure the app seeded manager account and rerun.')
