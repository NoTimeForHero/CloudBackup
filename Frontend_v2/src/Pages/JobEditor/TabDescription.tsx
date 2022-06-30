import { Job } from '../../api/types';
import { JobEditorProps } from './index';


const TabDescription = (props: JobEditorProps) => {

  const { edited: job } = props;

  return <div>

    <div className="form-group">
      <label>Название задачи</label>
      <input type="text" className="form-control" placeholder="Job Name №1" defaultValue={job?.Name} />
    </div>

    <div className="form-group">
      <label>Описание задачи</label>
      <textarea className="form-control" rows={3}>{job?.Details.description}</textarea>
    </div>
  </div>

}

export default TabDescription;