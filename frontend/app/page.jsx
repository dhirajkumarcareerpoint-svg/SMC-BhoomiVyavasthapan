import Link from "next/link"

export default function HomePage() {
  return (
    <div className="home-page">
      <section className="home-hero" aria-labelledby="home-title">
        <img className="home-hero-image" src="/smc-building-hd.jpg" alt="सोलापूर महानगरपालिका इमारत" />
        <div className="home-hero-content">
          <div className="home-emblem" aria-hidden="true">सो</div>
          <div>
            <p className="home-kicker">सोलापूर महानगरपालिका</p>
            <h1 id="home-title">भूमी व मालमत्ता व्यवस्थापन प्रणाली</h1>
            <p className="home-hero-copy">नागरिकांसाठी सुलभ, पारदर्शक आणि आधुनिक सेवा व्यवस्था</p>
          </div>
        </div>
      </section>

      <section className="home-action-area" aria-label="नागरिक सेवा" style={{ gap: 16 }}>
        <article className="home-demand-card">
          <div className="home-demand-icon" aria-hidden="true">📝</div>
          <div className="home-demand-content">
            <h2>मागणी अर्ज</h2>
            <p>सेवा, जागा आणि अर्जदाराची माहिती एकाच अर्जातून सादर करा</p>
          </div>
          <Link className="btn btn-primary home-demand-button" href="/demand-application">अर्ज करा</Link>
        </article>
        <article className="home-demand-card">
          <div className="home-demand-icon" aria-hidden="true">🔎</div>
          <div className="home-demand-content"><h2>अर्जाची स्थिती तपासा</h2><p>अर्ज क्रमांक टाकून आपल्या अर्जाची सद्यस्थिती तपासा</p></div>
          <Link className="btn btn-primary home-demand-button" href="/application-status">स्थिती तपासा</Link>
        </article>
      </section>

      <footer className="home-footer">
        <span>© 2026 सोलापूर महानगरपालिका. सर्व हक्क राखीव.</span>
        <span>गोपनीयता धोरण <b>|</b> नियम व अटी <b>|</b> संपर्क</span>
      </footer>
    </div>
  )
}
