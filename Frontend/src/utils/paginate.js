export function paginate(items, requestedPage, pageSize = 10) {
  const page = Math.max(1, Math.min(requestedPage, Math.max(1, Math.ceil(items.length / pageSize))));
  return { page, items: items.slice((page - 1) * pageSize, page * pageSize) };
}
