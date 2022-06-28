import { useState } from 'preact/compat';
import Alert from '../Components/Alert';
import { renderError, useAsyncEffect, wrapLoadable } from '../utils';
import Progress from '../Components/Progress';
import { getLogs, LogLine } from '../api';
import styles from './Logs.module.css';
import { LogLevel } from '../types';

const RowColor = (type: LogLevel) => {
  switch (type) {
    case 'TRACE':
    case 'DEBUG':
      return styles.log_debug;
    case 'INFO':
      return styles.log_info;
    case 'WARN':
      return styles.log_warn;
    case 'ERROR':
      return styles.log_error;
    case 'FATAL':
      return `${styles.log_error} ${styles.bold}`;
    default:
      console.warn('Unknown log line type', type);
      return '';
  }
}

const LogEntry = ({entry} : {entry: LogLine}) => {
  const [ date, level, logger, message ] = entry;
  return <tr className={RowColor(level)}>
    <td>{date}</td>
    <td>{level}</td>
    <td>{logger}</td>
    <td>{message}</td>
  </tr>
}

const Logs = () => {

  const [error, setError] = useState<any>();
  const [loading, setLoading] = useState(false);
  const [logs, setLogs] = useState<LogLine[]>([]);

  const loadLogs = () => wrapLoadable(async() => setLogs(await getLogs()), { setLoading, setError });
  useAsyncEffect(loadLogs, []);

  return <div className="mt-3">

    {error && <Alert message={renderError(error)} type={'danger'} />}
    {loading && <Progress />}

    <div>
      <button className="m-2 btn btn-secondary" onClick={loadLogs}>Обновить логи</button>
    </div>

    <div className="col-12 logs m-2">
      <table className="table">
        <tr className="thead-dark">
          <th>DateTime</th>
          <th>LogLevel</th>
          <th>Logger</th>
          <th>Message</th>
        </tr>
        {logs.map((entry, index) => <LogEntry key={index} entry={entry} />)}
      </table>
    </div>

  </div>

}

export default Logs;