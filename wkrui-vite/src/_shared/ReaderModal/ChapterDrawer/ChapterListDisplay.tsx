import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import MyInputStar from '_shared/Editors/MyInputStar';
import { ChapterVM } from '_utils/Types';
import { Col, Dropdown, Menu, Row, Typography } from 'antd';
import cssVariables from '_assets/styles/cssVariables';

export default function ChapterListDisplay(props: {
  chapters: ChapterVM[],
  currentPageIndex: number,

  getResizedPageSrc: (pageUncPath: string) => string,

  onRename: (title: string) => void,
  onDelete: (title: string) => void,
  onChapterClick: (pageIndex: number) => void,
  onTierChange: (chapterName: string, tier: number) => void
}){
  const { chapters } = props;

  return(
    <div style={{ paddingTop: "10px", paddingBottom:"10px" }}>
      {chapters.map((chapter, index) =>{ 
        const isCurrentlyViewed = props.currentPageIndex >= chapter.pageIndex
          && (index === chapters.length - 1
            || props.currentPageIndex < chapters[index + 1].pageIndex);

        const chapterProgress = props.currentPageIndex - chapter.pageIndex  + 1;

        const rowStyle = isCurrentlyViewed ? {
          backgroundColor: cssVariables.highlightMid
        } : {};

        return (
          <Row style={{ ...rowStyle, padding:'2px 24px 2px 24px' }} key={chapter.title}>
            <Dropdown overlay={
              <Menu>
                <Menu.Item key="1" onClick={() => props.onRename(chapter.title)}><EditOutlined />Rename</Menu.Item>
                <Menu.Item key="2" onClick={() => props.onDelete(chapter.title)}><DeleteOutlined />Delete</Menu.Item>
              </Menu>}
              trigger={['contextMenu']}
            >
              <Col span={8} onClick={() => props.onChapterClick(chapter.pageIndex)}>
                <img
                  style={{ objectFit: "contain", maxWidth: "100%", maxHeight: "100px", border: "1px solid white" }}
                  src={props.getResizedPageSrc(chapter.pageUncPath)}
                  alt="img"
                >
                </img>
              </Col>
            </Dropdown>
            <Col span={1}></Col>
            <Col span={15}>
              <Row>
                <Col span={24}>
                  <Typography.Text>{chapter.title}</Typography.Text>
                </Col>
                <Col span={24} style={{ paddingLeft:'10px' }}>
                  <MyInputStar value={chapter.tier} colSpan={{ label:0, control:24 }} onChange={(label, value) => { props.onTierChange(chapter.title, value); }}  />
                </Col>
                <Col span={24}>
                  <Typography.Text>{isCurrentlyViewed ? `${chapterProgress}/` : ''}{chapter.pageCount}</Typography.Text>
                </Col>
              </Row>
            </Col>
          </Row>
        )}
      )}
    </div>
  );
}
