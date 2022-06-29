export interface Plugin {
  Id: string,
  Name: string,
  Description: string,
  Url: string,
  Author: string,
  Version: string
}

export interface SystemResponse {
  Message: string
}

export type JobList = Record<string, Job>;

export interface Job {
  Id: string,
  Name: string,
  Details: JobDetails,
  State: JobState
}

export interface JobDetails {
  description?: string,
  copyTo?: string,
  nextLaunch?: string,
  jobsAfter?: string[],
  runAfter?: string,
}

export interface JobState {
  status: string,
  inProgress: boolean,
  isBytes: boolean,
  current: number,
  total: number
}