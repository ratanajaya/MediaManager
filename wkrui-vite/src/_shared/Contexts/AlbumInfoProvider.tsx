import { AlbumInfoVm } from '_utils/Types';
import { ReactNode, createContext, useContext } from 'react';
import _uri from '_utils/_uri';
import useSWR from 'swr';

const AlbumInfoContext = createContext<AlbumInfoVm | null>(null);

export function useAlbumInfo() {
  const albumInfo = useContext(
    AlbumInfoContext
  );

  return albumInfo;
}

export default function AlbumInfoProvider(props: { 
  children: ReactNode
}) {
  const { data: albumInfo, error: aiError } = useSWR<AlbumInfoVm>(_uri.GetAlbumInfo());
  
  return (
    <AlbumInfoContext.Provider
      value={albumInfo ?? null}
    >
      {props.children}
    </AlbumInfoContext.Provider>
  );
}

