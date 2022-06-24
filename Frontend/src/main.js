import App from './App.svelte';
import { createSockets } from './socket.js'; 

let app = null;

(async()=>{
	const settings = await fetch('/settings.json').then(x => x.json());
	//window.ax = SocketClient;
	createSockets(settings);
	app = new App({
		target: document.body,
		props: {
			settings
		}
	});
})();

export default app;