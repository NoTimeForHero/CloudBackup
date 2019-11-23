<script>
export let settings;

const shutdownClass = settings.isService ? 'disabled' : '';
const shutdownTitle = settings.isService ? 'Приложение запущено как служба и не может быть остановлено отсюда!' : ''
const shutdownClick = async() => {
    if (settings.isService) return;
    const res = await fetch(settings.apiUrl + '/shutdown', { method: 'DELETE'}).then(x => x.json());
    alert(res.Message);
}

</script>

<style>
</style>

<div class="row mt-4">
    <div class="col-12">

    <button class="btn btn-danger {shutdownClass}" title="{shutdownTitle}" on:click="{shutdownClick}">Завершить приложение</button>

    </div>
</div>