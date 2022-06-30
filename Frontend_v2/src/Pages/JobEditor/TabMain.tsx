import { Job } from '../../api/types';
import { JobEditorProps } from './index';


const TabMain = (props: JobEditorProps) => {

  const { edited: job } = props;

  return <div>

    <div className="form-group">
      <label>Уникальный идентификатор задачи</label>
      <input type="text" className="form-control" placeholder="job_example_id_1" defaultValue={job?.Id} />
    </div>

    {job && <div className="form-group mb-4">
        <button class="btn btn-danger">Удалить задачу</button>
    </div>}

    <div className="form-group">
      <label>Пароль для задачи</label>
      {/* TODO: Отдельный элемент управления для ввода пароля: */}
      {/* 1. Имеет переключатель отображения пароля */}
      {/* 2. Имеет кнопку генерации случайного пароля */}
      <input type="password" className="form-control" placeholder="some_long_password" />
    </div>

  </div>

}

export default TabMain;