-- 1. Remove all FAX-numbers, email, etc. from organizations
begin transaction
select * from clo_OrganizationContact_TB
update clo_OrganizationContact_TB
set ContactName = CAST(NEWID() as nvarchar(36))
select * from clo_OrganizationContact_TB
commit transaction

-- 2. Remove all FAX-numbers, email, etc. from holders
begin transaction
select * from clo_HolderContact_TB
update clo_HolderContact_TB
set ContactName = CAST(NEWID() as nvarchar(36))
select * from clo_HolderContact_TB
commit transaction