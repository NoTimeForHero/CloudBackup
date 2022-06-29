import { Job } from '../../api/types';
import TabMain from './TabMain';
import { useContext, useMemo, useState } from 'preact/compat';
import { cx } from '../../utils';
import { appContext } from '../../Components/App';
import TaskManager from '../TaskManager';
import StepButtons from './StepButtons';

export interface JobEditorProps {
  edited?: Job
}

interface Step {
  id: string,
  icon?: string,
  name: string,
  component: (props: JobEditorProps) => JSX.Element,
}

const steps : Step[] = [
  {id: 'primary', icon: 'fa-file-text-o', name: 'Основное', component: (props) => <TabMain {...props} />},
  {id: 'triggers', icon: 'fa-clock-o', name: 'Триггеры', component: (props) => <TabMain {...props} />},
  {id: 'masks', icon: 'fa-folder-open-o', name: 'Маски', component: (props) => <TabMain {...props} />},
  {id: 'controls', icon: 'fa-check-square-o', name: 'Управление', component: (props) => <TabMain {...props} />},
]

const JobEditor = (props: JobEditorProps) => {
  const { edited } = props;

  const [stepIndex, setStepIndex] = useState(0);
  const stepBody = useMemo(() => steps[stepIndex].component(props), [stepIndex]);
  const context = useContext(appContext);

  const onCancel = () => {
    context.setBody(<TaskManager />);
  }

  const onSave = () => {
    context.setBody(<TaskManager />);
  }

  return <div>
    <h5>{edited?.Name ?? 'Новая задача'}</h5>

    <ul className="nav nav-tabs mt-4 mb-4">
      {steps.map((step, index) => (
        <li className="nav-item" key={step.id}>
          <a className={cx('nav-link', {active: index == stepIndex})}
             onClick={(ev) => {ev.preventDefault(); setStepIndex(index)}}
             href="#">
            <i className={`fa ${step.icon} mr-2`} aria-hidden="true" />
            {step.name}
          </a>
        </li>
      ))}
    </ul>

    {stepBody}

    <StepButtons current={stepIndex}
                 max={steps.length}
                 onCancel={onCancel}
                 onSave={onSave}
                 onStepChange={(x) => setStepIndex(stepIndex + x)} />

  </div>
}

export default JobEditor;