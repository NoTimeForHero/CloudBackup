import { JobEditorProps } from './index';
import { cx } from '../../utils';
import { useState } from 'preact/compat';


const Mark = ({visible} : {visible?: boolean}) => (
  <span className="text-success">
    <i className={cx("fa fa-check mr-2",visible ? "visible" : "invisible")} aria-hidden="true" />
  </span>
)

const TabTriggers = (props: JobEditorProps) => {

  const { edited: job } = props;

  const [cron,setCron] = useState('');

  return <div>
    <div className="form-group">
      <label>
        <Mark visible={cron.length > 0} />
        Расписание CRON
      </label>
      {/* TODO: Исправить это убожество */}
      <input type="text" className="form-control" placeholder="" onInput={(ev) => setCron((ev.target as any)?.value)} />
    </div>

    <div className="form-group">
      <label>
        <Mark visible={false} />
        Запускать после задачи
      </label>
      <select className="form-control">
        <option>----- Отключено  ------</option>
        <option>Job 1</option>
        <option>Job 2</option>
      </select>
    </div>

  </div>

}

export default TabTriggers;