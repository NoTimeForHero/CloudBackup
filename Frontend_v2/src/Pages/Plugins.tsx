import { useState } from 'preact/compat';
import { getPlugins} from '../api';
import Alert from '../Components/Alert';
import { renderError, useLoadable} from '../utils';
import Progress from '../Components/Progress';
import { Plugin } from '../api/types';

const Plugins = () => {

  const [error, setError] = useState<any>();
  const [loading, setLoading] = useState(false);
  const [plugins, setPlugins] = useState<Plugin[]>([]);

  useLoadable(
    async() => setPlugins(await getPlugins()),
    { setLoading, setError }
  );

  return <div>

    {error && <Alert message={renderError(error)} type={'danger'} />}
    {loading && <Progress />}

    <div className="col-12 logs m-2">
      <table className="table">
        <tr className="thead-dark">
          <th>ID</th>
          <th>Название</th>
          <th>Описание</th>
        </tr>
        {plugins.map((plugin) => <tr key={plugin.Id}>
          <td>{plugin.Id}</td>
          <td>
            <span className="name">{plugin.Name}</span>
            <br/>
            (Версия {plugin.Version})
          </td>
          <td>{plugin.Description}</td>
        </tr>)}
      </table>
    </div>
  </div>
}

export default Plugins;