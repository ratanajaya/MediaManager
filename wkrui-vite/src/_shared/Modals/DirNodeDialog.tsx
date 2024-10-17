import { useState, useEffect } from 'react';
import { Modal, Button, Tree, Space } from 'antd';
import _uri from '_utils/_uri';
import { FsNode } from '_utils/Types';
import { DataNode } from 'antd/lib/tree';
import { useAuth } from '_shared/Contexts/useAuth';
import useNotification from '_shared/Contexts/NotifProvider';

const { DirectoryTree } = Tree;

interface DirNodeDialogProps{
  open: boolean,
  onClose: () => void,
  onOk: (selected?: string) => void
}

export default function DirNodeDialog(props: DirNodeDialogProps) {
  const [nodes, setNodes] = useState<FsNode[]>([]);
  const [selected, setSelected] = useState<string>();
  const { axiosA } = useAuth();
  const { notif } = useNotification();

  useEffect(() => {
    axiosA.get<FsNode[]>(_uri.GetLibraryDirNodes(1, '', true))
      .then((response) => {
        setNodes(response.data);
      })
      .catch((error) => {
        notif.apiError(error);
      });
  },[]);

  return (
    <Modal
      open={props.open}
      footer={null}
      centered={true}
      closable={false}
      onCancel={() => props.onClose()}
      style={{ width: 400 }}
    >
      <Space direction='vertical' style={{width:'100%'}}>
        <div style={{overflow:'auto', maxHeight:'90vh'}}>
          <MyDirTree 
            nodes={nodes}
            onSelect={setSelected}
          />
        </div>
        <Button
          style={{ width: "100%" }} type="primary"
          onClick={() => { props.onOk(selected); props.onClose(); }}
        >
          Move Here
        </Button>
      </Space>
    </Modal>
  );
}

function MyDirTree(props: {
  nodes: FsNode[],
  onSelect: (key: string) => void,
}){
  function toDataNode(fsNode: FsNode):DataNode {
    return {
      title: fsNode.alRelPath,
      key: fsNode.alRelPath,
      isLeaf: fsNode.dirInfo?.childs?.length === 0,
      children: !fsNode.dirInfo?.childs ? [] : fsNode.dirInfo.childs.map(child => toDataNode(child)),
      
    };
  }

  const dataNodes = props.nodes.map(fsNode => toDataNode(fsNode));

  return (
    <DirectoryTree 
      treeData={dataNodes}
      onSelect={(keys, info) => { if(keys.length > 0) props.onSelect(keys[0].toString()) }}
    />
  );
}