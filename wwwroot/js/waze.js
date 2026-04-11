window.waze = {
  open: function(addr) {
    try {
      var scheme = 'waze://?q=' + addr + '&navigate=yes';
      var web = 'https://www.waze.com/ul?q=' + addr + '&navigate=yes';
      // try open native app first, fallback to web after short timeout
      setTimeout(function(){ window.location.href = web; }, 800);
      window.location.href = scheme;
    } catch(e) {
      var web = 'https://www.waze.com/ul?q=' + addr + '&navigate=yes';
      window.open(web, '_blank');
    }
  }
};
