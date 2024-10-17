const IsMobile = () => {
  const width = window.innerWidth
  || document.documentElement.clientWidth
  || document.body.clientWidth;

  return width < 768;
}

const IsPhone = () => {
  const width = window.innerWidth
  || document.documentElement.clientWidth
  || document.body.clientWidth;

  return width < 480;
}

const IsScreenPortrait = () => {
  const height = window.innerHeight
  || document.documentElement.clientHeight
  || document.body.clientHeight;

  return height > window.innerWidth;
}

export { IsMobile, IsPhone, IsScreenPortrait };