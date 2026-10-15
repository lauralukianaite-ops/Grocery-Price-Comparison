import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "../../../components/ui/pagination";

export default function CatalogPagination({ page, pageCount, onPageChange }) {
  if (pageCount <= 1) {
    return null;
  }

  function changePage(event, nextPage) {
    event.preventDefault();
    onPageChange(nextPage);
  }

  return (
    <Pagination className="mt-10">
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            href="#products"
            aria-disabled={page === 1}
            className={page === 1 ? "pointer-events-none opacity-40" : ""}
            onClick={(event) => changePage(event, page - 1)}
          />
        </PaginationItem>
        {Array.from({ length: pageCount }, (_, index) => index + 1).map(
          (pageNumber) => (
            <PaginationItem key={pageNumber}>
              <PaginationLink
                href="#products"
                isActive={page === pageNumber}
                onClick={(event) => changePage(event, pageNumber)}
              >
                {pageNumber}
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
