import App from './App.svelte';

let app = null;

(async()=>{
	const settings = await fetch('/settings.json').then(x => x.json());
	app = new App({
		target: document.body,
		props: {
			settings
		}
	});
})();

export default app;