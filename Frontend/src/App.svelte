<script>
	export let settings;
	import TaskManager from './TaskManager.svelte';
	import Settings from './Settings.svelte';

	let menu = [		
		{ name: 'tasks', title: 'Задачи', component: TaskManager},
		{ name: 'logs', title: 'Логи', component: null},
		{ name: 'settings', title: 'Настройки', component: Settings}
	]
	let currentTab = menu[0];

	$: isMenuDisabled = (item) => currentTab.name === item.name ? 'disabled' : ''
	const setMenu = (item) => currentTab = item;
</script>

<main class="container">

	<nav class="navbar navbar-expand-lg navbar-light">
	<span class="navbar-brand">CloudBackup</span>
	<div class="collapse navbar-collapse" id="navbarNavAltMarkup">
		<div class="navbar-nav">
			{#each menu as item}
				<a class="nav-item nav-link {isMenuDisabled(item)}" on:click="{() => setMenu(item)}" href="javascript:">{item.title}</a>
			{/each}
		</div>
	</div>
	</nav>

	<svelte:component this={currentTab.component} settings={settings}/>
</main>

<style>
	.navbar {
		background-color: #e3f2fd;		
	}

	.navbar-brand {
		color: darkblue;
	}

	main {
		padding: 1em;
		margin: 0 auto;
	}
</style>