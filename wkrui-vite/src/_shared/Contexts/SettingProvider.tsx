import React, { ReactNode, createContext, useContext, useState } from "react";
import _ls from '_utils/_ls';
import { SettingVal } from "_utils/Types";
import _constant from "_utils/_constant";


interface SettingContextType {
  setSetting: React.Dispatch<React.SetStateAction<SettingVal>>,
  saveSetting:() => void,
  setting: SettingVal,
}

const SettingContext = createContext<SettingContextType | null>(null);

export function useSetting() {
  const context = useContext(
    SettingContext
  );

  return {
    setting: context?.setting ?? _constant.defaultSetting,
    setSetting: context?.setSetting!,
    saveSetting: context?.saveSetting!
  }
}

export default function SettingProvider(props: { 
  children: ReactNode
}){
  const [setting, setSetting] = useState<SettingVal>(_ls.loadWithCache<SettingVal>(_constant.lsKey.setting) ?? _constant.defaultSetting);

  return (
    <SettingContext.Provider
      value={{
        setting,
        setSetting,
        saveSetting: () => {
          _ls.set(_constant.lsKey.setting, setting);
        }
      }}
    >
      {props.children}
    </SettingContext.Provider>
  );
}