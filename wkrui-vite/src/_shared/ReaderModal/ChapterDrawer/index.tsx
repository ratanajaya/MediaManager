import { useState, useMemo } from 'react';

import { Drawer } from 'antd';
import {
  ExclamationCircleOutlined
} from '@ant-design/icons';

import TextDialog from '_shared/Modals/TextDialog';
import _uri from '_utils/_uri';
import { ChapterVM, FsNode, NodeType } from '_utils/Types';
import { useAuth } from '_shared/Contexts/useAuth';
import ChapterListDisplay from './ChapterListDisplay';
import useNotification from '_shared/Contexts/NotifProvider';
import usePageManager from '_shared/Contexts/usePageManager';
import { useModal } from '_shared/Contexts/ModalProvider';

interface IChapterDrawerProps{
  albumPath: string,
  fsNodes: FsNode[],
  type: 0 | 1,
  visible: boolean,
  currentPageIndex: number,
  onClose: () => void,
  onChapterRenameSuccess: (chapterName: string, newChapterName: string) => void,
  onChapterDeleteSuccess: (chapterName: string) => void,
  onJumpToPage: (pageIndex: number) => void,
  onChapterTierChangeSuccess: (chapterName: string, tier: number) => void
}

export default function ChapterDrawer(props: IChapterDrawerProps) {
  const { axiosA } = useAuth();
  const { modal } = useModal();
  const { notif } = useNotification();
  const {pageManager} = usePageManager();

  const chapters = useMemo(() => {
    const newChapters: ChapterVM[] = [];
    let i = 0;

    props.fsNodes
      .forEach(fs => {
        if(fs.nodeType !== NodeType.Folder){
          i++;
          return;
        }

        const childLen = fs.dirInfo!.childs.length;
        if(childLen === 0)
          return;

        newChapters.push({
          title: fs.dirInfo!.name,
          tier: fs.dirInfo!.tier,
          pageIndex: i,
          pageCount: childLen,
          pageUncPath: `${props.albumPath}\\${fs.dirInfo!.childs[0].alRelPath}`
        });

        i += childLen;
      });

      return newChapters;
  }, [props.fsNodes]);

  const [renameModal, setRenameModal] = useState<{
    visible: boolean,
    initialValue: string,
    onOk: (value: string, initialValue: string) => void
  }>({
    visible: false,
    initialValue: '',
    onOk: (value: string, initialValue: string) => {
      const movObj = {
        overwrite: false,
        src: {
          albumPath: props.albumPath,
          alRelPath: initialValue
        },
        dst: {
          albumPath: props.albumPath,
          alRelPath: value
        }
      };

      pageManager.movePage(props.type, movObj, (response: any) => {
          props.onChapterRenameSuccess(initialValue, value);
        },(error: any) => {
          notif.apiError(error);
        });
    }
  });

  const updateChapterTier = (param: { 
    albumPath: string,
    chapterName: string,
    tier: number
  }) => {
    axiosA.post(_uri.UpdateAlbumChapter(props.type), param)
        .then(function (response) {
          props.onChapterTierChangeSuccess(param.chapterName, param.tier);
        })
        .catch(function (error) {
          notif.apiError(error);
        });
  };

  const handler = {
    rename: (chapterTitle: string) => {
      setRenameModal(prev => {
        prev.visible = true;
        prev.initialValue = chapterTitle;
        return{ ...prev };
      });
    },
    delete: (chapterTitle: string) => {
      modal.confirm({
        title: `Delete chapter ${chapterTitle}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   Yes   ',
        okType: 'danger',
        cancelText: '   No   ',
        onOk() {
          axiosA.delete<number>(_uri.DeleteAlbumChapter(props.type, props.albumPath, chapterTitle))
            .then(function (response) {
              props.onChapterDeleteSuccess(chapterTitle);
            })
            .catch(function (error) {
              notif.apiError(error);
            });
        },
        onCancel() {
        },
      });
    },
    tierChange:(chapterTitle: string, value: number) => {
      updateChapterTier({ 
        albumPath:props.albumPath, 
        chapterName:chapterTitle, 
        tier:value 
      });
    }
  };

  return (
    <>
    <Drawer
      placement="left"
      closable={false}
      onClose={props.onClose}
      open={props.visible}
      width={300}
    >
      <ChapterListDisplay 
        chapters={chapters}
        currentPageIndex={props.currentPageIndex}

        getResizedPageSrc={(pageUncPath: string) => _uri.StreamResizedImage(pageUncPath, 150, props.type)}

        onRename={handler.rename}
        onDelete={handler.delete}
        onChapterClick={props.onJumpToPage}
        onTierChange={handler.tierChange}
      />
      <TextDialog
        visible={renameModal.visible} 
        onClose={() => setRenameModal(prev => { prev.visible = false; return {...prev}; })}
        initialValue={renameModal.initialValue}
        onOk={renameModal.onOk}
      />
    </Drawer>
    </>
  );
}