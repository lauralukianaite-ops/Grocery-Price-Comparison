import { ArrowLeft } from "lucide-react";
import { Link } from "react-router-dom";
import { buttonVariants } from "../components/ui/button";
import { APP_ROUTES } from "../router/routes";

export default function NotFoundPage() {
  return (
    <main
      id="main-content"
      tabIndex="-1"
      className="mx-auto grid min-h-[calc(100vh-4rem)] w-full max-w-[var(--page-content-width)] place-items-center px-4 py-16 text-center sm:px-6 lg:px-8 xl:px-10"
    >
      <div>
        <p className="text-sm font-semibold text-muted-foreground">404</p>
        <h1 className="mt-3 text-4xl font-semibold tracking-tight">
          Page not found
        </h1>
        <p className="mt-4 text-muted-foreground">
          The page you requested does not exist or may have moved.
        </p>
        <Link
          className={buttonVariants({ className: "mt-7" })}
          to={APP_ROUTES.home}
        >
          <ArrowLeft aria-hidden="true" /> Return home
        </Link>
      </div>
    </main>
  );
}
