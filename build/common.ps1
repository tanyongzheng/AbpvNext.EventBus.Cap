$full = $args[0]

# 公共路径 
# 获取根目录完整路径
$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# 开发模式下的解决方案列表，主要包含类库项目和部分样例项目，.sln文件所在路径
$solutionPaths = @(
		"../"
	)

if ($full -eq "-f")
{
	# 完整构建需要的其他解决方案，譬如WPF项目
	$solutionPaths += (
		"../"
	) 
}else{ 
	Write-host ""
	Write-host ":::::::::::::: !!! You are in development mode !!! ::::::::::::::" -ForegroundColor red -BackgroundColor  yellow
	Write-host "" 
} 
