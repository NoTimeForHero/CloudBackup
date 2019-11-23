<script>
	import { onMount } from 'svelte';

	let jobs = [
		{name: 'TEST!'}
	];

	onMount(async()=> {
		const data = await fetch('http://localhost:3000/api').then(x => x.json());
		console.log(Array.isArray(data), data.length) // true, 2
		data.forEach(x => jobs.push(x));
		console.log(jobs, jobs.length) // [[],[],[]], 3
	});
</script>

<style>
	p {
		color: purple;
		font-size: 2em;
	}
</style>

{#each jobs as job}
<h1>Job: {job}</h1>
{/each}

<p>&lt;Placeholder for TaskManager&gt;</p>