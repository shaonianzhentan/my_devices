import socket, threading, json, requests, urllib, logging, time

_LOGGER = logging.getLogger(__name__)

from .const import DOMAIN

def setup(hass, config):
    cfg = config[DOMAIN]

    return True