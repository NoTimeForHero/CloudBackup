import { useMemo, useState } from 'preact/compat';
import { cx } from '../../../utils';

interface Step {
  id: string,
  icon?: string,
  name: string,
  component: () => JSX.Element,
}

const steps : Step[] = [
  {id: 'files', icon: 'fa-file-o', name: 'Файлы', component: () => <div>Files</div>},
  {id: 'folders', icon: 'fa-folder-open-o', name: 'Каталоги', component: () => <div>Folders</div>},
  {id: 'preview', icon: 'fa-eye', name: 'Предпросмотр', component: () => <div>Preview</div>},
]

const TabMasks = () => {

  const [activeStep, setActiveStep] = useState(steps[0]);
  const body = useMemo(() => activeStep.component(), [activeStep]);

  return <div>
    <div className="form-group">
      <label>Каталог для резервного копирования</label>
      <div className="input-group">
        <input type="text" className="form-control" placeholder="" />
        <div className="input-group-append">
          <button className="input-group-text btn btn-primary">
            <i className="fa fa-folder-open-o" aria-hidden="true" />
          </button>
        </div>
      </div>
    </div>

    <ul className="nav nav-tabs mt-4 mb-4">
      {steps.map((step, index) => (
        <li className="nav-item" key={step.id}>
          <a className={cx('nav-link', {active: step.id == activeStep.id})}
             onClick={(ev) => {ev.preventDefault(); setActiveStep(step)}}
             href="#">
            <i className={`fa ${step.icon} mr-2`} aria-hidden="true" />
            {step.name}
          </a>
        </li>
      ))}
    </ul>

    {body}
  </div>

}

export default TabMasks;