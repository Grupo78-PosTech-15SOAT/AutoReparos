output "vpc_id" {
  value       = module.vpc.vpc_id
  description = "The ID of the VPC"
}

output "private_subnet_ids" {
  value       = module.vpc.private_subnet_ids
  description = "IDs of the private subnets"
}

output "public_subnet_ids" {
  value       = module.vpc.public_subnet_ids
  description = "IDs of the public subnets"
}

output "cluster_name" {
  value       = module.eks.cluster_name
  description = "The Name of the EKS Cluster"
}

output "cluster_endpoint" {
  value       = module.eks.cluster_endpoint
  description = "The endpoint for your EKS Kubernetes API"
}

output "oidc_provider_url" {
  value       = module.eks.oidc_provider_url
  description = "OIDC Provider URL for IAM roles for service accounts (IRSA)"
}

output "ebs_csi_addon_arn" {
  value       = module.addons.ebs_csi_addon_arn
  description = "ARN of the EBS CSI driver addon"
}

output "ecr_repository_url" {
  value       = module.ecr.repository_url
  description = "The URL of the ECR repository"
}

