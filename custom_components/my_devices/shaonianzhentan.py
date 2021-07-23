import uuid, requests, urllib
from homeassistant.helpers import template
from homeassistant.components.http import HomeAssistantView
from homeassistant.helpers.network import get_url
from .const import VERSION, DOMAIN, DOMAIN_API

# 获取本机MAC地址
def get_mac_address_key(): 
    mac=uuid.UUID(int = uuid.getnode()).hex[-12:] 
    return "".join([mac[e:e+2] for e in range(0,11,2)])

# 解析模板
def template_message(_message):
    tpl = template.Template(_message, HASS)
    _message = tpl.async_render(None)
    return _message

class DeviceServer:

    def __init__(self, hass, host, web_url, mqtt_host):
        self.hass = hass
        self.api_url = f"http://{host}:8124"
        self.web_url = web_url
        self.mqtt_host = mqtt_host
        self.ha_api = get_url(hass).strip('/') + DOMAIN_API

    def set_value(key, value):
        # 文本转语音
        if key == 'tts':
            value = urllib.parse.quote(template_message(value))
        elif key == 'url':
            value = urllib.parse.quote(value)
        res = requests.get(self.api_url + '/set?key=' + key + '&value=' + str(value))
        print(res.json())

    # 连接MQTT
    def connect(self):
        self.set_value('mqtt', self.mqtt_host)
        self.set_value('ha_api', self.ha_api)
        self.set_value('url', self.web_url)

class HassView(HomeAssistantView):

    url = DOMAIN_API
    name = DOMAIN
    requires_auth = False
    
    async def get(self, request):
        # 这里进行重定向
        hass = request.app["hass"]
        if 'text' in request.query:
            text = request.query['text']
            # 触发事件
            hass.async_create_task(hass.services.async_call('conversation', 'process', {'text': text}))
            return self.json({'code': '0', 'msg': text})
        else:
            return self.json({'code': '401', 'msg': '参数不正确'})