import { Job } from '../../api/types';

interface JobEditorProps {
  edited?: Job
}

const JobEditor = (props: JobEditorProps) => {
  const { edited } = props;
  return <div>
    <h1>JobEditor: {edited?.Name ?? '<New job>'}</h1>
  </div>
}

export default JobEditor;