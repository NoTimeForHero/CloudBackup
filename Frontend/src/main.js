import App from './App.svelte';

const settings = window.settings;
const app = new App({
	target: document.body,
	props: {
		settings
	}
});

export default app;