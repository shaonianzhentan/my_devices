from .shaonianzhentan import get_mac_address_key

DOMAIN = "my_devices"
DEFAULT_NAME = "我的设备"
VERSION = "1.0"
DOMAIN_API = f'/{DOMAIN}-api-{get_mac_address_key()}'