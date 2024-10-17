import Flags from 'country-flag-icons/react/3x2';

export default function MyFlagIcon(props : {
  flagType: string
}) {
  const flagStyle = { width: "18px", height: "12px", marginRight: "1px", marginBottom: "1px" };
  const borderRadius = "0px 5px 5px 0px";

  return props.flagType === "English" 
    ? <Flags.GB style={flagStyle} /> 
    : props.flagType === "Japanese" 
    ? <Flags.JP style={flagStyle} /> 
    : props.flagType === "Chinese" 
    ? <Flags.CN style={flagStyle} /> 
    : props.flagType === "Other" 
    ? <Flags.ID style={flagStyle} /> 
    : props.flagType === "New" 
    ? <div style={{ ...flagStyle, borderRadius:borderRadius, backgroundColor:"hsla(60, 70%, 60%, 1)" }} /> 
    : props.flagType === "Wip" 
    ? <div style={{ ...flagStyle, borderRadius:borderRadius, backgroundColor:"hsla(120, 70%, 60%, 1)" }} /> 
    : props.flagType === "Source" 
    ? <div style={{ ...flagStyle, borderRadius:borderRadius, backgroundColor:"hsla(180, 70%, 60%, 1)" }} /> 
    : <></>
}
