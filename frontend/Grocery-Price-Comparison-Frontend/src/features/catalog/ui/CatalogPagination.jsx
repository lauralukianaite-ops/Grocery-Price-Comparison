import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "../../../components/ui/pagination";

function getVisiblePages(currentPage, totalPages) {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const pages = [1];
  const start = Math.max(2, currentPage - 1);
  const end = Math.min(totalPages - 1, currentPage + 1);

  if (start > 2) {
    pages.push("ellipsis-start");
  }

  for (let p = start; p <= end; p += 1) {
    pages.push(p);
  }

  if (end < totalPages - 1) {
    pages.push("ellipsis-end");
  }

  pages.push(totalPages);
  return pages;
}

export default function CatalogPagination({ page, pageCount, onPageChange }) {
  if (pageCount <= 1) {
    return null;
  }

  function changePage(event, nextPage) {
    event.preventDefault();
    onPageChange(nextPage);
  }

  const visiblePages = getVisiblePages(page, pageCount);

  return (
    <Pagination className="mt-10">
      <PaginationContent className="flex-wrap justify-center">
        <PaginationItem>
          <PaginationPrevious
            href="#products"
            aria-disabled={page === 1}
            className={page === 1 ? "pointer-events-none opacity-40" : ""}
            onClick={(event) => changePage(event, page - 1)}
          />
        </PaginationItem>
        {visiblePages.map((item) =>
          typeof item === "string" ? (
            <PaginationItem key={item}>
              <PaginationEllipsis />
            </PaginationItem>
          ) : (
            <PaginationItem key={item}>
              <PaginationLink
                href="#products"
                isActive={page === item}
                onClick={(event) => changePage(event, item)}
              >
                {item}
              </PaginationLink>
            </PaginationItem>
          ),
        )}
        <PaginationItem>
          <PaginationNext
            href="#products"
            aria-disabled={page === pageCount}
            className={
              page === pageCount ? "pointer-events-none opacity-40" : ""
            }
            onClick={(event) => changePage(event, page + 1)}
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
}
