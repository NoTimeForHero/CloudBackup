import { onMount, onDestroy } from 'svelte';

export class SocketClient {

    constructor(settings, isDebug = false) {
        this.events = {
            'error': new Set(),
            'message': new Set(),
            'open': new Set(),
            'close': new Set(),
            'any': new Set(),
        }
        this.socket = undefined;
        this.reconnectTimer = undefined;
        this.reconnectInterval = 5000;
        this.settings = settings;
        this.isActive = false;
        this.isDebug = isDebug;

		const rawUrl = new URL(this.settings.apiUrl);
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
            if (this.isDebug) console.log('SocketClient', 'error', ev.data);            
            this.events.error.forEach((handler) => handler(ev));            
            this.events.any.forEach((handler) => handler(this.isActive));
            this.reconnectTimer = setTimeout(this.makeSocketClient.bind(this), this.reconnectInterval);            
		}
        this.socket.onclose = (ev) => {
            this.isActive = false;
            if (this.isDebug) console.log('SocketClient', 'close', ev.data);                        
            this.events.close.forEach((handler) => handler(ev));            
            this.events.any.forEach((handler) => handler(this.isActive));              
            this.reconnectTimer = setTimeout(this.makeSocketClient.bind(this), this.reconnectInterval);
        }
	}

	destroy() {
        console.log('SocketClient', 'WebSocket closed...');
        clearTimeout(this.reconnectTimer);
        this.socket.close();
	}    
}

export const createSockets = (settings) => {
    window.client = new SocketClient(settings);     
}

export const useSockets = (eventName, callback) => {
    /** @type {SocketClient} */
    const client = window.client;

    if (!(eventName in client.events)) throw new Error('Unknown event: ' + eventName);
    /** @type {Set} */
    const target = client.events[eventName];

	onMount(()=> {
        // console.log('useSockets->onMount');
        target.add(callback);
	});

	onDestroy(()=> {
        // console.log('useSockets->onDestroy');        
        target.delete(callback);
	});    
}