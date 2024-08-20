import Heading from '@/app/components/Heading'
import React from 'react'
import AutionForm from '../AuctionForm'

export default function Create() {
  return (
    <div className='mx-auto max-w-[75%] shadow-lg bg-white rounded-lg'>
      <Heading title='Sell your car' subtitle='Enter details' />
      <AutionForm />
    </div>
  )
}
