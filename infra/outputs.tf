output "vm_public_ip" {
  description = "Public IP of the demo VM"
  value       = azurerm_public_ip.vm.ip_address
}

output "demo_url" {
  description = "Demo URL"
  value       = "https://${var.domain_name}"
}
