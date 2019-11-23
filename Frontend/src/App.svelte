<script>
	import TaskManager from './TaskManager.svelte';

	let menu = [		
		{ name: 'tasks', title: 'Задачи', component: TaskManager},
		{ name: 'logs', title: 'Логи', component: null}
	]
	let currentTab = menu[0];

	$: isMenuDisabled = (item) => currentTab.name === item.name ? 'disabled' : ''
	const setMenu = (item) => currentTab = item;
</script>

<main>

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

	<svelte:component this={currentTab.component}/>
</main>

<style>
	.navbar {
		background-color: #e3f2fd;		
	}

	.navbar-brand {
		color: darkblue;
	}

	main {
		text-align: center;
		padding: 1em;
		margin: 0 auto;
	}
</style>