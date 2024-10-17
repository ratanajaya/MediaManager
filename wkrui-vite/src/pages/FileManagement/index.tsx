import { Typography } from 'antd'

export default function FileManagement(props:{
  path: string,
  type: 0 | 1
}) {
  return (
    <Typography.Title>WIP File Management: {props.path}</Typography.Title>
  )
}
