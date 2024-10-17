import { useState } from 'react';
import { ForceGraphData, ForceGraphLink, ForceGraphNode } from '_utils/dashboardTypes';
import { useQuery } from '@tanstack/react-query';
import { ForceGraph3D } from 'react-force-graph';
import SpriteText from 'three-spritetext';
import RangeSlider from 'react-bootstrap-range-slider';
import { useAuth } from '_shared/Contexts/useAuth';
import _uri from '_utils/_uri';

export default function TagGraph() {
  const [minLinkValue, setMinLinkValue] = useState(0.2);

  const { axiosA } = useAuth();

  const { isPending, error, data } = useQuery<ForceGraphData>({
    queryKey: ['tagForce'],
    queryFn: ({signal}) => 
      axiosA.get<ForceGraphData>(_uri.GetTagForceGraphData(), { signal }).then(res => res.data)
  })

  if (isPending) return <div>'Loading...'</div>;
  if (error) return (<div>{`An error has occurred: ${error.message}`}</div>);

  const filteredData:ForceGraphData = {
    nodes: data.nodes,
    links: data.links.filter(a => a.value >= minLinkValue)
  }

  const maxValue = filteredData.links.reduce((max, link) => (link.value > max ? link.value : max), 0);
  
  return (
    <>
      <div style={{display:'flex'}}>
        <div style={{padding:'8px'}}>
          Threshold :
        </div>
        <div style={{flex:'1'}}>
          <RangeSlider
            value={minLinkValue}
            min={0}
            max={1}
            step={0.05}
            onChange={changeEvent => setMinLinkValue(parseFloat(changeEvent.target.value))}
          />
        </div>
      </div>

      <div style={{height:'12px'}}></div>

      <div style={{width:'100%'}}>
        <ForceGraph3D
          height={800}
          graphData={filteredData}
          nodeThreeObject={(node: ForceGraphNode) => {
            const sprite = new SpriteText(node.id);
              sprite.textHeight = 8;
              return sprite;
          }}
          linkColor={(link: ForceGraphLink) => {
            const scaledValue = link.value / maxValue;

            return `rgba(255,255,0, ${(0 + (1 * scaledValue))})`;
          }}
          linkWidth={0.5}
          linkOpacity={1}
          nodeLabel='count'
          linkLabel={(link:any) => { 
            const pctSrc = (link.linkCount / link.sourceCount * 100).toFixed(1);
            const pctTar = (link.linkCount / link.targetCount * 100).toFixed(1);

            return `${link.source.id} (${pctSrc}% / ${link.sourceCount}) | ${link.target.id} (${pctTar}% / ${link.targetCount})`;
          }}
        />
      </div>
    </>
  )
}
