import uuid

# 获取本机MAC地址
def get_mac_address_key(): 
    mac=uuid.UUID(int = uuid.getnode()).hex[-12:] 
    return "".join([mac[e:e+2] for e in range(0,11,2)])

DOMAIN = "my_devices"
DEFAULT_NAME = "我的设备"
VERSION = "1.0"
DOMAIN_API = f'/{DOMAIN}-api-{get_mac_address_key()}'