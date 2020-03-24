$version = Read-Host -Prompt 'Version number (<major>.<minor>.<build>)'

git tag -a v$version -m "Release of $version"
git push origin v$version