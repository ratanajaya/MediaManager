import React, { useMemo } from 'react';
import { Drawer } from 'antd';
import { ChapterVM, FsNode, NodeType } from '_utils/Types';
import ChapterListDisplay from '_shared/ReaderModal/ChapterDrawer/ChapterListDisplay';

interface IOfflineChapterDrawerProps{
  albumPath: string,
  fsNodes: FsNode[],
  visible: boolean,
  currentPageIndex: number,
  onClose: () => void,
  onJumpToPage: (pageIndex: number) => void,
  getCoverPageSrc: (pageUncPath: string) => string
}

export default function OfflineChapterDrawer(props: IOfflineChapterDrawerProps) {
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

  return (
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
        getResizedPageSrc={props.getCoverPageSrc}

        onRename={() => {}}
        onDelete={() => {}}
        onChapterClick={props.onJumpToPage}
        onTierChange={() => {}}
      />
    </Drawer>
  );
}