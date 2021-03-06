import socket, threading, json, requests, urllib, logging, time

_LOGGER = logging.getLogger(__name__)

from .shaonianzhentan import DeviceServer, HassView
from .const import DOMAIN, VERSION

def setup(hass, config):
    cfg = config[DOMAIN]
    # MQTT配置
    mqtt = cfg.get('mqtt', {})
    devices = cfg.get('device', [])
    # 安装
    for item in devices:
        host = item['host']
        url = item.get('url', '').replace('TIMESTAMP', str(int(time.time())))
        domain_key = f"{DOMAIN}{host}"
        hass.data[domain_key] = DeviceServer(hass, host, url, mqtt['host'])

    # 设置数据
    def setting_data(call):
        data = call.data
        domain_key = f"{DOMAIN}{data.get('ip', '')}"
        if domain_key in hass.data:
            dev = hass.data[domain_key]

            for key in data:
                if key == 'ip':
                    continue
                value = data.get(key)
                if value is not None:
                    dev.set_value(key, value)

    # 订阅服务
    hass.services.async_register(DOMAIN, 'setting', setting_data)
    # 注册事件网关
    hass.http.register_view(HassView)
    # 显示插件信息
    _LOGGER.info('''
-------------------------------------------------------------------

    我的设备【作者QQ：635147515】
    
    版本：''' + VERSION + '''
    
-------------------------------------------------------------------''')
    # 接收信息
    def udp_socket_recv_client():
        udp_socket = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
        udp_socket.bind(("", 9234))
        while True:
            try:
                recv_data, recv_addr = udp_socket.recvfrom(1024)
                host = recv_addr[0]
                data = json.loads(recv_data.decode('utf-8'))
                print(data)
                # 设置启动页面
                ip = data.get('ip', '')
                if ip != '':
                    domain_key = f"{DOMAIN}{ip}"
                    print(domain_key)
                    if domain_key in hass.data:
                        dev = hass.data[domain_key]
                        print(dev)
                        dev.connect()
            except Exception as ex:
                print(ex)

    # 监听广播
    socket_recv_thread = threading.Thread(target=udp_socket_recv_client,args=())
    socket_recv_thread.start()
    return True