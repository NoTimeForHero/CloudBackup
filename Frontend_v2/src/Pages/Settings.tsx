import { useState } from 'preact/compat';
import Alert, { AlertProps } from '../Components/Alert';
import { wait } from '../utils';
import { makeReload, makeShutdown} from '../api';
import { BootstrapStyle } from '../types';
import { SystemResponse } from '../api/types';


const Settings = () => {

  const [alert, setAlert] = useState<AlertProps|undefined>();

  const doAction = async(action: () => Promise<SystemResponse>, type: BootstrapStyle, hideDelay: number) => {
    const data = await action();
    setAlert({type, message: data.Message});
    await wait(hideDelay).then(() => setAlert(undefined));
  }

  const shutdownClick = () => doAction(makeShutdown, 'danger', 2000);
  const reloadClick = () => doAction(makeReload, 'info', 2000);

  return <div>

    {alert && <Alert {...alert} />}

    <button className="btn btn-danger mr-3" title="{shutdownTitle}" disabled={false} onClick={shutdownClick}>
      Завершить приложение
    </button>
    <button className="btn btn-warning" onClick={reloadClick}>
      Перезагрузить конфигурацию
    </button>

  </div>
}

export default Settings;