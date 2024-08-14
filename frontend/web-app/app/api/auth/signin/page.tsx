import EmptyFilter from '@/app/components/EmptyFilter'
import React from 'react'

export default function Page({searchParams}: {searchParams: {callbackUrl: string}}) {
  return (
    <EmptyFilter
        title='You need to logged in'
        subtitle='Please click below'
        showLogin
        callbackUrl={searchParams.callbackUrl}
    />
  )
}
