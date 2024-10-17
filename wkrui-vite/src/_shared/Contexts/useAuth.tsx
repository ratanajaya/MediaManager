import _constant from "_utils/_constant";
import axios from "axios";
import _ls from "_utils/_ls";
import _helper from "_utils/_helper";
import { useMemo, useState } from "react";

export function useAuth() {
  const [token, setToken] = useState<string | null>(_ls.loadWithCache<string>(_constant.lsKey.authToken));

  const isAuthenticated = !_helper.isNullOrEmpty(token);

  const logout = () => {
    _ls.remove(_constant.lsKey.authToken);
    setToken(null);
    window.location.href="#/login";
  };

  //Plain instance
  const axiosE = axios.create();
  
  const [axiosA, fetcherA] = useMemo(() =>{
    return [
      axios.create({
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }),
      (url: string) => {
        return axiosA.get(url)
          .then(res => res.data);
      }
    ]
  }, [token]);

  return {
    isAuthenticated,
    logout,
    token,
    setToken,
    axiosE,
    axiosA,
    fetcherA
  }
}