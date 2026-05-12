# Warehouse Onboarding Notes (Raw Example)

> This is a **raw requirement example**.
> It is intentionally written in business-facing language and includes ambiguity.
> The next step is to convert it into an engineering spec such as [`../../specs/example-warehouse-create.md`](../../specs/example-warehouse-create.md).

## Background

Right now, new warehouse locations are usually tracked through spreadsheets and ad hoc chat messages. Operations wants a more formal way to register a warehouse before stock starts moving in or out.

## What the business wants

- Back-office users should be able to create a warehouse record.
- The warehouse should have a code, a name, a location, and some kind of capacity value.
- We want to stop duplicate warehouse records from being created by accident.
- After a warehouse is created, other teams should be able to look it up.

## Likely users

- operations team
- warehouse admins
- maybe supervisors or managers later

## Rough flow

1. User enters warehouse details.
2. System saves the warehouse.
3. User can see the created warehouse afterward.

## Things people mentioned verbally

- Warehouse code should probably be unique.
- Capacity should not be blank and should not be zero.
- Some people think inactive warehouses should not be allowed at creation time.
- Someone mentioned there may be an approval step in the future, but not necessarily now.
- It would be good if we can tell who created the warehouse.

## Questions that were not fully answered

- Should duplicate warehouse code return a validation error or a conflict response?
- Is uniqueness global or tenant-based?
- Do viewers get read-only access immediately, or only admins at first?
- Is approval out of scope for now, or should we leave room for it in the API shape?

## Expected outcome

- Users can create a warehouse without using spreadsheets.
- Users can retrieve the created warehouse record later.
- We reduce duplicate setup mistakes.

## Non-technical acceptance language

- "It should be easy for operations to create a warehouse."
- "We should know who created it."
- "Bad data should be blocked."
- "Other people should be able to find the warehouse later."

## Out of scope hints

- no bulk import for now
- no stock initialization yet
- delete behavior not discussed
- approval workflow not confirmed
