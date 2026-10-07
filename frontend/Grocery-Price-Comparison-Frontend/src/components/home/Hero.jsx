export default function Hero() {
  return (
    <section
      className="relative flex min-h-screen items-start bg-background px-6 pt-[clamp(4rem,12vh,8rem)] pb-8"
      aria-labelledby="hero-title"
    >
      <div className="absolute inset-x-0 bottom-0 h-[22%] bg-muted" />
      <div className="relative z-10 mx-auto max-w-3xl text-center">
        <h1
          id="hero-title"
          className="m-0 text-[clamp(2.5rem,7vw,5.5rem)] leading-[0.96] font-medium tracking-[-0.065em] text-foreground"
        >
          Find the better grocery price.
        </h1>
      </div>
    </section>
  );
}
