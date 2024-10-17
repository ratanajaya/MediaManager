import _uri from "_utils/_uri";
import { Button, Input, Spin, Tabs, Typography, Modal } from "antd";
import React, { useState } from "react";
import { Comment, AlbumVM, Source, SourceAndContent, SourceAndContentUpsertModel, SourceDeleteModel, SourceUpdateModel } from '_utils/Types';
import moment from "moment";
import { useAuth } from "_shared/Contexts/useAuth";
import _helper from "_utils/_helper";
import useSWR from 'swr';
import useNotification from "_shared/Contexts/NotifProvider";

export default function CommentPanel(props: {
  open: boolean,
  albumVm: AlbumVM,
  setAlbumVM: React.Dispatch<React.SetStateAction<AlbumVM>>,
}){
  const { axiosA } = useAuth();
  const [loading, setLoading] = useState<boolean>(false);
  const [editedSource, setEditedSource] = useState<Source | null>(null);
  const { notif } = useNotification();
  
  const handlers = {
    addSource: (source: Source) => {
      alert('Unimportant feature. Not yet implemented');
      return;

      // props.setAlbumVM(prev => {
      //   if(prev.album.sources.some(a => a.url === source.url)){
      //     return prev;
      //   }

      //   prev.album.sources = [
      //     ...prev.album.sources,
      //     {
      //       url: _helper.removeHttpsAndTrailingSlash(source.url),
      //       title: source.title,
      //       subTitle: null,
      //     }
      //   ]
      //   return {...prev};
      // });
    },
    updateSource: (source: Source) => {
      const param: SourceUpdateModel = {
        albumPath: props.albumVm.path,
        source: source
      }

      setLoading(true);

      axiosA.post(_uri.UpdateSource(), param)
        .then(res => {
          props.setAlbumVM(prev => {
            const updateIdx = prev.album.sources.findIndex(a => a.url === source.url);
            prev.album.sources[updateIdx] = source;

            return {...prev};
          })
        })
        .catch((err) => {
          notif.apiError(err);
        })
        .finally(() => {
          setLoading(false);
          setEditedSource(null);
        });
    },
    deleteSource: (url: string) => {
      const param: SourceDeleteModel = {
        albumPath: props.albumVm.path,
        url: url
      }

      setLoading(true);

      axiosA.post(_uri.DeleteSource(), param)
        .then(res => {
          props.setAlbumVM(prev => {
            prev.album.sources = prev.album.sources.filter(a => a.url !== url);

            return {...prev};
          })
        })
        .catch((err) => {
          notif.apiError(err);
        })
        .finally(() => {
          setLoading(false);
          setEditedSource(null);
        });
    },
    addSourceAndContent: (newSourceAndContent: SourceAndContent) => {
      const param: SourceAndContentUpsertModel = {
        albumPath: props.albumVm.path,
        sourceAndContent: newSourceAndContent
      }

      setLoading(true);
      axiosA.post(_uri.UpsertSourceAndContent(), param)
        .then(res => {
          props.setAlbumVM(prev => {
            const foundIdx = prev.album.sources.findIndex(a => a.url === newSourceAndContent.source.url);

            if(foundIdx >= 0){
              prev.album.sources[foundIdx] = newSourceAndContent.source;
            }
            else{
              prev.album.sources = [
                ...prev.album.sources,
                newSourceAndContent.source
              ];
            }

            return {...prev};
          });
        })
        .catch((err) => {
          notif.apiError(err);
        })
        .finally(() => setLoading(false));
    }
  }

  return(
    <Spin spinning={loading}>
      <div style={{width:'100%'}}>
        <Tabs 
          type="card"
          size="small"
          items={[
            {
              key:'1',
              label: 'JSON',
              children: <SourceAndContentJsonCustomizer
                onFinish={handlers.addSourceAndContent}
                apiError={notif.apiError}
              />
            },
            {
              key:'2',
              label: 'Custom',
              children: <SourceCustomizer 
                onFinish={handlers.addSource}
              />
            }
          ]}
        />
        <div className="divider-8"></div>
        <div>
          <Tabs 
            type="card"
            size="small"
            items={props.albumVm.album.sources.sort((a, b) => _helper.nz(a.subTitle, a.url).localeCompare(_helper.nz(b.subTitle, b.url)))
              .map((a, i) => {
              return {
                key: a.url,
                label: <span onContextMenu={async (e) => {
                      e.preventDefault();
                      setEditedSource(a);
                    }}>
                    {_helper.limitStringToNCharsWithTrailingPad(_helper.nz(a.subTitle, a.url), 8, '~')}
                  </span>,
                children: <CommentTabContent source={a} />
              }
            })}
          />
        </div>
        <Modal
          open={editedSource != null}
          footer={null}
          centered={true}
          closable={false}
          onCancel={() => setEditedSource(null)}
          style={{ maxWidth: 400, textAlign: "center" }}
        >
          <SourceCustomizer 
            source={editedSource ?? undefined}
            onFinish={handlers.updateSource}
            onDelete={handlers.deleteSource}
          />
        </Modal>
      </div>
    </Spin>
  );
}

function CommentTabContent(props: {
  source: Source
}){
  const { data: comments, error } = useSWR<Comment[], any>(_uri.GetComments(props.source.url));

  if (error) { return <Typography.Text>Error!</Typography.Text>; }
  if (!comments) { return <Typography.Text>Loading comments...</Typography.Text>; }

  return(
    <>
      <span className='comment-timestamp'>
        {props.source.title}
      </span>
      <div className="divider-8"></div>
      <div style={{
        maxHeight: '520px',
        overflowY: 'auto'
      }}>
        {comments.map(a => (
            <CommentBox key={a.id} comment={a} />
          ))
        }
      </div>
      <div className="divider-8"></div>
    </>
  )
}

function CommentBox(props: {
  comment: Comment
}){
  const comment = props.comment;

  return(
    <div className='comment-box'>
      <div className='comment-author'>
        {comment.author}
      </div>
      <div className='divider-2' />
      <div style={{display:'flex', justifyContent:'space-between'}}>
        <span className='comment-timestamp'>
          {moment(comment.postedDate).format('yyyy-MMM-DD, HH:mm')}
        </span>
        {comment.score != null &&
          <span className='comment-score' style={{color: (comment.score < 0 ? 'red' : 'lime')}}>
            {comment.score < 0 ? '' : '+'}{comment.score}
          </span>
        }
      </div>
      <div className='divider-6' />
      <div className='comment-content'
        dangerouslySetInnerHTML={{ __html: comment.content}}
      />
    </div>
  );
}

function SourceCustomizer(props:{
  source?: Source,
  onFinish: (source: Source) => void,
  onDelete?: (url: string) => void,
}){
  const [newSource, setNewSource] = useState<Source>(props.source ?? {
    title: '',
    url: '',
    subTitle: null,
  });

  return(
    <div style={{display:'flex', width:'100%'}}>
      <div style={{flex:'1'}}>
        <Input
          style={{
            width: '100%',
          }}
          value={newSource.url}
          placeholder="URL"
          onChange={(event) => setNewSource(prev => {
            prev.url = event.target.value;
            return {...prev};
          })}
          disabled={props.source != null}
        />
        <div className="divider-8"></div>
        <Input
          style={{
            width: '100%',
          }}
          value={newSource.title ?? ''}
          placeholder="Title"
          onChange={(event) => setNewSource(prev => {
            prev.title = event.target.value;
            return {...prev};
          })}
        />
        <div className="divider-8"></div>
        <Input
          style={{
            width: '100%',
          }}
          value={newSource.subTitle ?? ''}
          placeholder="Subtitle"
          onChange={(event) => setNewSource(prev => {
            prev.subTitle = event.target.value;
            return {...prev};
          })}
        />
      </div>
      <div style={{width:'8px'}}></div>
      <div style={{width:'60px', display:'flex', flexDirection:'column'}}>
        <Button
          style={{width:'100%', flex:'1'}} 
          onClick={() => props.onFinish(newSource)}>
            UPS
        </Button>
        {props.onDelete &&
          <Button
            style={{width:'100%', flex:'1'}} 
            onClick={() => props.onDelete!(props.source?.url ?? '')}>
              DEL
          </Button>
        }
      </div>
    </div>
  );
}

function SourceAndContentJsonCustomizer(props: {
  onFinish: (snc: SourceAndContent) => void,
  apiError: (error: any) => void
}){
  const [newSourceAndContent, setNewSourceAndContent] = useState<SourceAndContent>({
    source: {
      title: null,
      subTitle: null,
      url: ''
    },
    comments: []    
  });

  function pasteSourceCommentJson(event: React.ChangeEvent<HTMLInputElement>) {
    try{
      const scFromPastedJson = JSON.parse(event.target.value) as SourceAndContent;

      setNewSourceAndContent(prev => {
        return {
          ...scFromPastedJson,
          source: {
            ...scFromPastedJson.source,
            subTitle: prev.source.subTitle
          }
        }
      });
    }
    catch(error){
      props.apiError(error);
    }
  }

  return(
    <div style={{display:'flex', width:'100%'}}>
      <Input
        //Not a controlled component. For pasting only
        style={{
          flex: '1'
        }}
        placeholder="SC Json"
        onChange={pasteSourceCommentJson}
      />
      <div style={{width:'8px'}}></div>
      <Input
        style={{
          flex: '1'
        }}
        value={newSourceAndContent.source.title ?? ''}
        placeholder="Title"
        onChange={(event) => setNewSourceAndContent(prev => {
          prev.source.title = event.target.value;
          return {...prev};
        })}
      />
      <div style={{width:'8px'}}></div>
      <Input
        style={{
          flex: '1'
        }}
        value={newSourceAndContent.source.subTitle ?? ''}
        placeholder="Subtitle"
        onChange={(event) => setNewSourceAndContent(prev => {
          prev.source.subTitle = event.target.value;
          return {...prev};
        })}
      />
      <div style={{width:'8px'}}></div>
      <div style={{width:'60px'}}>
        <Button style={{height:'100%', width:'100%'}} 
          onClick={() => props.onFinish(newSourceAndContent)}>
            Add
        </Button>
      </div>
    </div>  
  )
}