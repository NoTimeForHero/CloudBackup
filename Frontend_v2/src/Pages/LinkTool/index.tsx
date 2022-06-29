import { Job } from '../../api/types';

interface LinkToolProps {
  job: Job
}

const LinkTool = (props: LinkToolProps) => {
  const { job } = props;
  return <div>
    <h1>LinkTool: {job.Name}</h1>
  </div>
}

export default LinkTool;