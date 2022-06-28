import { useEffect } from 'preact/compat';
import { ExtendedWindow, Settings } from './types';

export type EventType = 'error'|'message'|'open'|'close'|'any';

export class SocketClient {

  socket?: WebSocket;
  reconnectTimer: number|undefined;
  reconnectInterval = 5000;
  events: Record<EventType, Set<Function>>;
  connectUrl: string;
  isActive: boolean;
  isDebug: boolean;

  constructor(settings: Settings, isDebug = false) {
    this.events = {
      'error': new Set(),
      'message': new Set(),
      'open': new Set(),
      'close': new Set(),
      'any': new Set(),
    }
    this.isActive = false;
    this.isDebug = isDebug;

    const rawUrl = new URL(settings.apiUrl);
    this.connectUrl = `ws://${rawUrl.host}/ws-status`;

    this.makeSocketClient();
  }

  makeSocketClient() {
    if (this.isDebug) console.log('SocketClient', 'connecting', this.connectUrl);
    this.socket = new WebSocket(this.connectUrl);
    this.socket.onopen = () => {
      this.isActive = true;
      if (this.isDebug) console.log('SocketClient', 'open');
      this.events.open.forEach((handler) => handler());
      this.events.any.forEach((handler) => handler(this.isActive));
    }
    this.socket.onmessage = (ev) => {
      this.isActive = true;
      //console.log('SocketClient', 'message', ev.data);
      this.events.message.forEach((handler) => handler(ev.data, ev));
      this.events.any.forEach((handler) => handler(this.isActive));
    }
    this.socket.onerror = (ev) => {
      this.isActive = false;
      if (this.isDebug) console.log('SocketClient', 'error', ev);
      this.events.error.forEach((handler) => handler(ev));
      this.events.any.forEach((handler) => handler(this.isActive));
    }
    this.socket.onclose = (ev) => {
      this.isActive = false;
      if (this.isDebug) console.log('SocketClient', 'close', ev);
      this.events.close.forEach((handler) => handler(ev));
      this.events.any.forEach((handler) => handler(this.isActive));
      this.reconnectTimer = setTimeout(this.makeSocketClient.bind(this), this.reconnectInterval);
    }
  }

  destroy() {
    console.log('SocketClient', 'WebSocket closed...');
    clearTimeout(this.reconnectTimer);
    this.socket?.close();
  }
}

export const createSockets = (settings: Settings, isDebug = false) => {
  (window as ExtendedWindow).client = new SocketClient(settings, isDebug);
}

export const useSockets = (eventName: EventType, callback: Function) => {
  const client = (window as ExtendedWindow).client;

  if (!client) throw new Error(`Missing SocketClient in window!`);
  if (!(eventName in client.events)) throw new Error('Unknown event: ' + eventName);

  const target = client.events[eventName];
  useEffect(() => {
    target.add(callback);
    return () => target.delete(callback);
  });
}