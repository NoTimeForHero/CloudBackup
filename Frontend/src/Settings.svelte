<script>
export let settings;

let alert = null;

const shutdownClass = settings.isService ? 'disabled' : '';
const shutdownTitle = settings.isService ? 'Приложение запущено как служба и не может быть остановлено отсюда!' : ''
const shutdownClick = async() => {
    if (settings.isService) return;
    const res = await fetch(settings.apiUrl + '/shutdown', { method: 'DELETE'}).then(x => x.json());
    alert = { class: 'danger', message: res.Message }    
    setTimeout(() => alert = null, 3000);
}


const reloadClick = async() => {
    const res = await fetch(settings.apiUrl + '/reload', { method: 'GET'}).then(x => x.json());
    alert = { class: 'info', message: res.Message }    
    setTimeout(() => alert = null, 3000);
}
</script>

<style>
    .nope {
        visibility: hidden;
    }
</style>

<div class="row mt-4">
    <div class="col-12">

        <div class="alert {alert ? `alert-${alert.class}` : 'nope'}" role="alert">
            {alert ? alert.message : '&nbsp;'}
        </div>	

        <button class="btn btn-danger {shutdownClass}" title="{shutdownTitle}" on:click="{shutdownClick}">Завершить приложение</button>
        <button class="btn btn-warning" on:click="{reloadClick}">Перезагрузить конфигурацию</button>

    </div>
</div>