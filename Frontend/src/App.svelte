<script>
export let settings;
import Logs from './Logs.svelte';
import TaskManager from './TaskManager.svelte';
import Plugins from './Plugins.svelte';	
import Settings from './Settings.svelte';
import HealthCheck from './HealthCheck.svelte';
import { onMount } from 'svelte';

let menu = [		
	{ name: 'tasks', title: 'Задачи', component: TaskManager},
	{ name: 'logs', title: 'Логи', component: Logs},
	{ name: 'plugins', title: 'Плагины', component: Plugins },		
	{ name: 'settings', title: 'Настройки', component: Settings},
]
let currentTab = menu[0];

$: isMenuDisabled = (item) => currentTab.name === item.name ? 'active' : ''
const setMenu = (item) => {
	document.location.hash = item.name;
	currentTab = item;
}

const initialize = async() => {
	const hash = document.location.hash;
	if (!hash) return;
	const needed = menu.find(x => `#${x.name}` === hash);
	if (needed) setMenu(needed);
}

onMount(initialize);	
</script>

<main class="container">

	<nav class="navbar navbar-light">
	<div class="navbar-brand">
		<HealthCheck settings={settings} />
		<span class="ml-3">{settings.appName}</span>
	</div>
	<div id="navbarNavAltMarkup">
		{#each menu as item}
			<a class="nav-item nav-link {isMenuDisabled(item)}" on:click="{() => setMenu(item)}" href={`#${item.name}`}>{item.title}</a>
		{/each}
	</div>
	</nav>

	<svelte:component this={currentTab.component} settings={settings}/>
</main>

<style>
	.navbar {
		background-color: #e3f2fd;		
	}

	.navbar-brand {
		display: flex;
		color: darkblue;
		align-items: center;
	}

	main {
		padding: 1em;
		margin: 0 auto;
	}

	.active {
		text-decoration: underline;
	}

	#navbarNavAltMarkup {
		display: flex;
		flex-direction: row;
		/* background: blue; */
	}
</style>